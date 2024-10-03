// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "This is testing the entire collection interface.", Scope = "namespaceanddescendants", Target = "~N:Xledger.Collections.Test")]
[assembly: SuppressMessage("Assertions", "xUnit2017:Do not use Contains() to check if a value exists in a collection", Justification = "This is testing the entire collection interface.", Scope = "namespaceanddescendants", Target = "~N:Xledger.Collections.Test")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "Polyfill.>", Scope = "namespace", Target = "~N:System.Runtime.CompilerServices")]
