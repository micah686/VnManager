// Copyright (c) micah686. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using VnManager.ViewModels.Dialogs.ModifyGame;
// ReSharper disable CheckNamespace

namespace VnManager.Interfaces
{
    public interface IModifyGameHostFactory
    {
        ModifyGameHostViewModel CreateModifyGameHost();
    }
}
