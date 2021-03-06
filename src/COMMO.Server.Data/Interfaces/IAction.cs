// <copyright file="IAction.cs" company="2Dudes">
// Copyright (c) 2018 2Dudes. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using COMMO.Server.Data.Models.Structs;

namespace COMMO.Server.Data.Interfaces
{
    public interface IAction
    {
        IPacketIncoming Packet { get; }

        Location RetryLocation { get; }

        IList<IPacketOutgoing> ResponsePackets { get; }

        void Perform();
    }
}