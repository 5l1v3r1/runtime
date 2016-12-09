//------------------------------------------------------------------------------
// <copyright file="DomainControllerCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.DirectoryServices.ActiveDirectory {
	using System;
	using System.Collections;
	using System.Globalization;

	public class DomainControllerCollection: ReadOnlyCollectionBase {

		internal DomainControllerCollection() { }		

		internal DomainControllerCollection(ArrayList values) {
			if (values != null) {
				InnerList.AddRange(values);
			}
		}

		public DomainController this[int index] {
			get {
				return (DomainController)InnerList[index];                                
			}
		}

		public bool Contains(DomainController domainController) {

                     if (domainController == null)
				throw new ArgumentNullException("domainController");
            
			for (int i = 0; i < InnerList.Count; i++) {
				DomainController tmp = (DomainController)InnerList[i];
				if (Utils.Compare(tmp.Name, domainController.Name) == 0) {
					return true;
				}
			}
			return false;
		}                                 

		public int IndexOf(DomainController domainController) {

                      if (domainController == null)
				throw new ArgumentNullException("domainController");
            
			for (int i = 0; i < InnerList.Count; i++) {
				DomainController tmp = (DomainController)InnerList[i];
				if (Utils.Compare(tmp.Name, domainController.Name) == 0) {
					return i;
				}
			}
			return -1;
		}     

		public void CopyTo(DomainController[] domainControllers, int index) {
			InnerList.CopyTo(domainControllers, index);
		}
	}
}
