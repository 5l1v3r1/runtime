//------------------------------------------------------------------------------
// <copyright file="ReadOnlyActiveDirectorySchemaClassCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.DirectoryServices.ActiveDirectory {
	using System;
	using System.Globalization;
	using System.Collections;

	public class ReadOnlyActiveDirectorySchemaClassCollection: ReadOnlyCollectionBase {
		
		internal ReadOnlyActiveDirectorySchemaClassCollection() { }

		internal ReadOnlyActiveDirectorySchemaClassCollection(ICollection values) {
			if (values != null) {
				InnerList.AddRange(values);
			}
		}

		public ActiveDirectorySchemaClass this[int index] {
			get {
				return (ActiveDirectorySchemaClass)InnerList[index];                                   
			}
		}

		public bool Contains(ActiveDirectorySchemaClass schemaClass) {
            
                     if (schemaClass == null)
				throw new ArgumentNullException("schemaClass");
            
			for (int i = 0; i < InnerList.Count; i++) {
				ActiveDirectorySchemaClass tmp = (ActiveDirectorySchemaClass)InnerList[i];
				if (Utils.Compare(tmp.Name, schemaClass.Name) == 0) {
					return true;
				}
			}
			return false;
		}                                  

		public int IndexOf(ActiveDirectorySchemaClass schemaClass) {

                     if (schemaClass == null)
				throw new ArgumentNullException("schemaClass");
            
			for (int i = 0; i < InnerList.Count; i++) {
				ActiveDirectorySchemaClass tmp = (ActiveDirectorySchemaClass)InnerList[i];
				if (Utils.Compare(tmp.Name, schemaClass.Name) == 0) {
					return i;
				}
			}
			return -1;
		}     

		public void CopyTo(ActiveDirectorySchemaClass[] classes, int index) {
			InnerList.CopyTo(classes, index);
		}
	}
}
