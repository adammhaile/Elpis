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
using Util;

namespace PandoraSharp.Exceptions
{
    public class PandoraException : Exception
    {
        public PandoraException(ErrorCodes fault, Exception innerException) :
            base(Errors.GetErrorMessage(fault), innerException)
        {
            Fault = fault;
        }

        public PandoraException(ErrorCodes fault)
            : base(Errors.GetErrorMessage(fault))
        {
            Fault = fault;
        }

        public ErrorCodes Fault { get; set; }
        public string FaultMessage { get { return Errors.GetErrorMessage(Fault); } }
    }

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