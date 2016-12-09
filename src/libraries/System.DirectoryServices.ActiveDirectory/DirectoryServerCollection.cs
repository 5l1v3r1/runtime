//------------------------------------------------------------------------------
// <copyright file="ReadOnlyDirectoryServerCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
/*
 */
namespace System.DirectoryServices.ActiveDirectory {
	using System;
	using System.Runtime.InteropServices;
	using System.Collections;
	using System.Globalization;
	using System.Diagnostics;

	public class DirectoryServerCollection: CollectionBase {
		internal string siteDN = null;
		internal string transportDN = null;
		internal DirectoryContext context = null;
		internal bool initialized = false;
		internal Hashtable changeList = null;
		private ArrayList copyList = new ArrayList();
		private DirectoryEntry crossRefEntry = null;
		private bool isADAM = false;
		bool isForNC = false;

		internal DirectoryServerCollection(DirectoryContext context, string siteDN, string transportName) {
			Hashtable tempTable = new Hashtable();

			changeList = Hashtable.Synchronized(tempTable);
			this.context = context;
			this.siteDN = siteDN;
			this.transportDN = transportName;
		}

		internal DirectoryServerCollection(DirectoryContext context, DirectoryEntry crossRefEntry, bool isADAM, ReadOnlyDirectoryServerCollection servers) {
			this.context = context;
			this.crossRefEntry = crossRefEntry;
			this.isADAM = isADAM;

			isForNC = true;
			foreach (DirectoryServer server in servers) {
				InnerList.Add(server);
			}
			
			
		}

		public DirectoryServer this[int index] {
			get {
				return (DirectoryServer)InnerList[index];
			}
			set {	
				DirectoryServer server = (DirectoryServer)value;

				if (server == null)
					throw new ArgumentNullException("value");

				if (!Contains(server))
					List[index] = server;
				else
					throw new ArgumentException(Res.GetString(Res.AlreadyExistingInCollection, server), "value");
			}
		}

		public int Add(DirectoryServer server) {
			if (server == null)
				throw new ArgumentNullException("server");

			// make sure that it is within the current site
			if (isForNC) {

				if ((!isADAM)) {

					if (!(server is DomainController))
						throw new ArgumentException(Res.GetString(Res.ServerShouldBeDC), "server");
					
					// verify that the version >= 5.2
					// DC should be Win 2003 or higher
					if (((DomainController)server).NumericOSVersion < 5.2) {
						throw new ArgumentException(Res.GetString(Res.ServerShouldBeW2K3), "server");
					}
				}
	
				if (!Contains(server)) {
					return List.Add(server);
				}
				else {
					throw new ArgumentException(Res.GetString(Res.AlreadyExistingInCollection, server), "server");
				}
			}
			else {
                                string siteName = (server is DomainController) ? ((DomainController)server).SiteObjectName : ((AdamInstance)server).SiteObjectName;
				Debug.Assert(siteName != null);
				if (Utils.Compare(siteDN, siteName) != 0) {
					throw new ArgumentException(Res.GetString(Res.NotWithinSite));
				}

				if (!Contains(server))
					return List.Add(server);
				else
					throw new ArgumentException(Res.GetString(Res.AlreadyExistingInCollection, server), "server");
			}
		}

		public void AddRange(DirectoryServer[] servers) {
			if (servers == null)
				throw new ArgumentNullException("servers");

			foreach (DirectoryServer s in servers) {
				if (s == null) {
					throw new ArgumentException("servers");
				}
			}

			for (int i = 0; ((i) < (servers.Length)); i = ((i) + (1)))
				this.Add(servers[i]);
		}

		public bool Contains(DirectoryServer server) {

                     if (server == null)
				throw new ArgumentNullException("server");
            
			for (int i = 0; i < InnerList.Count; i++) {
				DirectoryServer tmp = (DirectoryServer)InnerList[i];

				if (Utils.Compare(tmp.Name, server.Name) == 0) {
					return true;
				}
			}
			return false;
		}

		public void CopyTo(DirectoryServer[] array, int index) {
			List.CopyTo(array, index);
		}

		public int IndexOf(DirectoryServer server) {

                     if (server == null)
				throw new ArgumentNullException("server");

            
			for (int i = 0; i < InnerList.Count; i++) {
				DirectoryServer tmp = (DirectoryServer)InnerList[i];

				if (Utils.Compare(tmp.Name, server.Name) == 0) {
					return i;
				}
			}
			return -1;
		}

		public void Insert(int index, DirectoryServer server) {
			if (server == null)
				throw new ArgumentNullException("server");

			if (isForNC) {

				if ((!isADAM)) {

					if (!(server is DomainController))
						throw new ArgumentException(Res.GetString(Res.ServerShouldBeDC), "server");
					
					// verify that the version >= 5.2
					// DC should be Win 2003 or higher
					if (((DomainController)server).NumericOSVersion < 5.2) {
						throw new ArgumentException(Res.GetString(Res.ServerShouldBeW2K3), "server");
					}
				}
				
				if (!Contains(server)) {
					List.Insert(index, server);
				}
				else {
					throw new ArgumentException(Res.GetString(Res.AlreadyExistingInCollection, server), "server");
				}
			}
			else {
				// make sure that it is within the current site
				string siteName = (server is DomainController) ? ((DomainController)server).SiteObjectName : ((AdamInstance)server).SiteObjectName;
				Debug.Assert(siteName != null);
				if (Utils.Compare(siteDN, siteName) != 0) {
					throw new ArgumentException(Res.GetString(Res.NotWithinSite), "server");
				}

				if (!Contains(server))
					List.Insert(index, server);
				else
					throw new ArgumentException(Res.GetString(Res.AlreadyExistingInCollection, server));
			}
		}

		public void Remove(DirectoryServer server) {

                     if (server == null)
				throw new ArgumentNullException("server");

            
			for (int i = 0; i < InnerList.Count; i++) {
				DirectoryServer tmp = (DirectoryServer)InnerList[i];

				if (Utils.Compare(tmp.Name, server.Name) == 0) {
					List.Remove(tmp);
					return;
				}
			}
			
			// something that does not exist in the collection
			throw new ArgumentException(Res.GetString(Res.NotFoundInCollection, server), "server");
		}

		protected override void OnClear() {
			if (initialized && !isForNC) {
				copyList.Clear();
				foreach (object o in List) {
					copyList.Add(o);
				}
			}
		}

		protected override void OnClearComplete() {
			// if the property exists, clear it out
			if (isForNC) {
				if (crossRefEntry != null) {

					try {
						if (crossRefEntry.Properties.Contains(PropertyManager.MsDSNCReplicaLocations)) {
							crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Clear();
						}
					}
					catch (COMException e) {
						throw ExceptionHelper.GetExceptionFromCOMException(context, e);
					}
				}
			}
			else if (initialized) {
				for (int i = 0; i < copyList.Count; i++) {
					OnRemoveComplete(i, copyList[i]);
				}
			}
		}

		protected override void OnInsertComplete(int index, object value) {
			if (isForNC) {
				if (crossRefEntry != null) {
					try {
						DirectoryServer server = (DirectoryServer)value;
                                      	string ntdsaName = (server is DomainController) ? ((DomainController)server).NtdsaObjectName : ((AdamInstance)server).NtdsaObjectName;
						crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Add(ntdsaName);
					}
					catch (COMException e) {
						throw ExceptionHelper.GetExceptionFromCOMException(context, e);
					}
				}
			}
			else if (initialized) {
				DirectoryServer server = (DirectoryServer)value;
				string name = server.Name;
                                string serverName = (server is DomainController) ? ((DomainController)server).ServerObjectName : ((AdamInstance)server).ServerObjectName;

                            try
                            {
                        		if (changeList.Contains(name)) {
                        			((DirectoryEntry)changeList[name]).Properties["bridgeheadTransportList"].Value = this.transportDN;
                        		}
                        		else {
                        			DirectoryEntry de = DirectoryEntryManager.GetDirectoryEntry(context, serverName);

                        			de.Properties["bridgeheadTransportList"].Value = this.transportDN;
                        			changeList.Add(name, de);
                        		}
                            }
                            catch (COMException e) 
                            {
                                throw ExceptionHelper.GetExceptionFromCOMException(context, e);
                            }
			}
		}

		protected override void OnRemoveComplete(int index, object value) {
			if (isForNC) {
				try {
					if (crossRefEntry != null) {
                                                string ntdsaName = (value is DomainController) ? ((DomainController)value).NtdsaObjectName : ((AdamInstance)value).NtdsaObjectName;
						crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Remove(ntdsaName);
					}
				}
				catch (COMException e) {
					throw ExceptionHelper.GetExceptionFromCOMException(context, e);
				}
			}
			else {
				DirectoryServer server = (DirectoryServer)value;
				string name = server.Name;
                                string serverName = (server is DomainController) ? ((DomainController)server).ServerObjectName : ((AdamInstance)server).ServerObjectName;

                            try
                            {
        				if (changeList.Contains(name)) {
        					((DirectoryEntry)changeList[name]).Properties["bridgeheadTransportList"].Clear();
        				}
        				else {
        					DirectoryEntry de = DirectoryEntryManager.GetDirectoryEntry(context, serverName);

        					de.Properties["bridgeheadTransportList"].Clear();
        					changeList.Add(name, de);
        				}
                            }
                            catch (COMException e) 
                            {
                                throw ExceptionHelper.GetExceptionFromCOMException(context, e);
                            }
			}
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue) {
			OnRemoveComplete(index, oldValue);
			OnInsertComplete(index, newValue);
		}

		protected override void OnValidate(Object value) {
			if (value == null) throw new ArgumentNullException("value");

			if (isForNC) {
				if (isADAM) {
					// for adam this should be an ADAMInstance
					if (!(value is AdamInstance))
						throw new ArgumentException(Res.GetString(Res.ServerShouldBeAI), "value");
				}
				else {
					// for AD this should be a DomainController
					if (!(value is DomainController))
						throw new ArgumentException(Res.GetString(Res.ServerShouldBeDC), "value");
				}
			}
			else {
				if (!(value is DirectoryServer))
					throw new ArgumentException("value");
			}
		}

		internal string[] GetMultiValuedProperty() {
			ArrayList values = new ArrayList();
			for (int i = 0; i < InnerList.Count; i++) {
				DirectoryServer ds = (DirectoryServer)InnerList[i];

				string ntdsaName = (ds is DomainController) ? ((DomainController)ds).NtdsaObjectName : ((AdamInstance)ds).NtdsaObjectName;
				values.Add(ntdsaName);
			}
			return (string[])values.ToArray(typeof(string));
		}
	}
}
