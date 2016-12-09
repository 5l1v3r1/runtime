/*--
Copyright (c) 2004  Microsoft Corporation

Module Name:

    ADDNConstraintLinkedAttrSet.cs

Abstract:

    Implements the ADDNConstraintLinkedAttrSet ResultSet class.
    
History:

    05-18-2007    TQuerec     Created

--*/


using System;
using System.DirectoryServices;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace System.DirectoryServices.AccountManagement
{
    // <SecurityKernel Critical="True" Ring="0">
    // <Asserts Name="Declarative: [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted = true)]" />
    // <ReferencesCritical Name="Type ADDNLinkedAttrSet" Ring="1" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [DirectoryServicesPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted=true)]
    class ADDNConstraintLinkedAttrSet : ADDNLinkedAttrSet
    {

        /// 
        /// <summary>
        /// Result validator - Delegate function signature.
        /// 
        /// The result validator should return true if the result is valid
        /// and false if the result is invalid and needs to be skipped.
        /// </summary>
        /// <param name="resultDirEntry">
        /// Directory entry object of the result.
        /// NOTE: ADDNLinkedAttrSet class is responsible for disposing this DirectoryEntry object.
        /// </param>
        /// <returns>
        /// True - if the result is valid (further processing with the result will happen in this case)
        /// False - If the result is invalid. In this case the result will skipped.
        /// </returns>
        /// 
        internal delegate bool ResultValidator(dSPropertyCollection resultPropCollection); 


        internal enum ConstraintType
        {
            ContainerStringMatch = 0,  // match = the objects distinguishedName begins with the string supplied as constraintData,  i.e.  the object is under that container.
            ResultValidatorDelegateMatch = 1, //match = the result object will be passed as an argument to the 
                                              //function supplied as constraintData and it should return true.
        }
        
        internal ADDNConstraintLinkedAttrSet(
                            ConstraintType constraint,
                            object constraintData,
                            string groupDN,
                            IEnumerable[] members,
                            string primaryGroupDN, 
                            DirectorySearcher queryMembersSearcher,
                            bool recursive, 
                            ADStoreCtx storeCtx) : base(groupDN, members, primaryGroupDN, queryMembersSearcher, recursive, storeCtx) 
        { 
            Debug.Assert(constraintData != null);
        
            this.constraint = constraint;
            this.constraintData = constraintData;
        }

        internal ADDNConstraintLinkedAttrSet(
                    ConstraintType constraint,
                    object constraintData,
                    string groupDN,
                    DirectorySearcher[] membersSearcher,
                    string primaryGroupDN,
                    DirectorySearcher primaryGroupMembersSearcher,
                    bool recursive,
                    ADStoreCtx storeCtx)
            : base(groupDN, membersSearcher, primaryGroupDN, primaryGroupMembersSearcher, recursive, storeCtx)
        {
            Debug.Assert(constraintData != null);

            this.constraint = constraint;
            this.constraintData = constraintData;
        }

       ConstraintType constraint;
       object constraintData;
       
    	override internal bool MoveNext()
        {
            GlobalDebug.WriteLineIf(GlobalDebug.Info, "ADDNConstraintLinkedAttrSet", "Entering MoveNext");
            GlobalDebug.WriteLineIf(GlobalDebug.Info, "ADDNConstraintLinkedAttrSet", "Filter {0}", constraintData);
            bool match = false;
            string dn = "NotSet";

            if ( !base.MoveNext() )
                return false;
            else
            {
                while (!match)
                {
                    if ( null == this.current )
                        return false;
                    
                    switch ( constraint )
                    {
                        case ConstraintType.ContainerStringMatch:

                            if ( this.current is SearchResult )
                                dn = ((SearchResult)this.current).Properties["distinguishedName"][0].ToString();
                            else
                                dn = ((DirectoryEntry)this.current).Properties["distinguishedName"].Value.ToString();
                            
                            if ( dn.EndsWith( (string)constraintData, StringComparison.Ordinal ))
                                match = true;
                            
                            break;
                        case ConstraintType.ResultValidatorDelegateMatch:
                            {
                                ResultValidator resultValidator = this.constraintData as ResultValidator;
                                if (resultValidator != null)
                                {
                                    dSPropertyCollection resultPropCollection = null;
                                    if (this.current is SearchResult)
                                    {
                                        resultPropCollection = new dSPropertyCollection(((SearchResult)this.current).Properties);
                                    }
                                    else
                                    {
                                        resultPropCollection = new dSPropertyCollection(((DirectoryEntry)this.current).Properties);
                                    }
                                    match = resultValidator.Invoke(resultPropCollection);
                                }
                                else
                                {
                                    Debug.Fail("ADStoreCtx.ADDNConstraintLinkedAttrSet: Invalid constraint data. Expected: object of type ResultValidator");
                                }
                                break;
                            }
                        default:
                            Debug.Fail("ADStoreCtx.ADDNConstraintLinkedAttrSet: fell off end looking for " + constraint.ToString());
                            break;

                    }

                    GlobalDebug.WriteLineIf(GlobalDebug.Info, "ADDNConstraintLinkedAttrSet", "Found {0} Match {1}", dn, match.ToString());

                    if ( !match )
                        if ( !this.MoveNext())
                            return false;
                }

                return match;
            }       
    	}
    }
}
