﻿using System;

namespace Xs;

/// <summary>
/// Represents an Xs state management handle that closes application if disposed.
/// </summary>
public class XsHandle : IDisposable
{
    /// <inheritdoc/>
    public void Dispose() => XsManager.Quit();
}