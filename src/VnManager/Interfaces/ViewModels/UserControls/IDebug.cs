// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using VnManager.ViewModels.UserControls;
// ReSharper disable CheckNamespace

namespace VnManager.Interfaces
{
    public interface IDebugFactory
    {
        DebugViewModel CreateDebug();
    }
}
