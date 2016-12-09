//------------------------------------------------------------------------------
// <copyright file="SecurityMasks.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.DirectoryServices{

    /// <include file='doc\SecurityMasks.uex' path='docs/doc[@for="SecurityMasks"]/*' />
    /// <devdoc>
    ///    <para>
    ///       Specifies the available options for examining security information of an object.
    ///    </para>
    /// </devdoc>
    [Flags]
    public enum SecurityMasks {
    
        /// <include file='doc\SecurityMasks.uex' path='docs/doc[@for="SecurityMasks.None"]/*' />
    	None = 0,
    	
    	/// <include file='doc\SecurityMasks.uex' path='docs/doc[@for="SecurityMasks.Owner"]/*' />
    	Owner = 1,

        /// <include file='doc\SecurityMasks.uex' path='docs/doc[@for="SecurityMasks.Group"]/*' />        
        Group = 2,

        /// <include file='doc\SecurityMasks.uex' path='docs/doc[@for="SecurityMasks.DACL"]/*' />
        Dacl = 4,

        /// <include file='doc\SecurityMasks.uex' path='docs/doc[@for="SecurityMasks.SACL"]/*' />
        Sacl = 8
   	}
 }
