/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of PandoraSharp.
 * PandoraSharp is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * PandoraSharp is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with PandoraSharp. If not, see http://www.gnu.org/licenses/.
*/

using System;

namespace PandoraSharp.Exceptions
{
    public class PandoraException : Exception
    {
        public PandoraException(string fault, Exception innerException) :
            base(fault, innerException)
        {
            FaultCode = fault;
        }

        public PandoraException(string fault) : base(fault)
        {
            FaultCode = fault;
        }

        public string FaultCode { get; set; }
    }

    //public class PandoraAuthTokenException : PandoraException
    //{
    //    public PandoraAuthTokenException(string msg, Exception innerException)
    //        : base(msg, innerException) { }

    //    public PandoraAuthTokenException(string msg)
    //        : base(msg) { }
    //}

    //public class PandoraNetException : PandoraException
    //{
    //    public PandoraNetException(string msg, Exception innerException)
    //        : base(msg, innerException) { }

    //    public PandoraNetException(string msg)
    //        : base(msg) { }
    //}

    //public class PandoraTimeoutException : PandoraNetException
    //{
    //    public PandoraTimeoutException(string msg, Exception innerException)
    //        : base(msg, innerException) { }

    //    public PandoraTimeoutException(string msg)
    //        : base(msg) { }
    //}

    public class XmlRPCException : Exception
    {
        public XmlRPCException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }

        public XmlRPCException(string msg)
            : base(msg)
        {
        }
    }
}