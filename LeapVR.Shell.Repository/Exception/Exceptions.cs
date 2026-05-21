#region Licence
/****************************************************************
 *  Filename: Exceptions.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Shell.Repository.Exception
{
    public class RepositorySerializationException : System.Exception
    {
        public RepositorySerializationException(string message):base(message)
        {}
        public RepositorySerializationException(string message, System.Exception innerException) : base(message, innerException)
        { }
        public RepositorySerializationException()
        { }
    }
    public class RepositoryStoreDbException : System.Exception
    {
        public RepositoryStoreDbException(string message) : base(message)
        { }
        public RepositoryStoreDbException(string message, System.Exception innerException) : base(message, innerException)
        { }
        public RepositoryStoreDbException()
        { }
    }
    public class RepositoryDeleteDbException : System.Exception
    {
        public RepositoryDeleteDbException(string message) : base(message)
        { }
        public RepositoryDeleteDbException(string message, System.Exception innerException) : base(message, innerException)
        { }
        public RepositoryDeleteDbException()
        { }
    }
    public class RepositoryGetDbException : System.Exception
    {
        public RepositoryGetDbException(string message) : base(message)
        { }
        public RepositoryGetDbException(string message, System.Exception innerException) : base(message, innerException)
        { }
        public RepositoryGetDbException()
        { }
    }
}
