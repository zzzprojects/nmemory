using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.Diagnostics;
using NMemory.Diagnostics.Messages;
using NMemory.Linq;
using NMemory.Modularity;
using NMemory.Execution;

namespace NMemory.Execution
{
    public class QueryCompiler : QueryCompilerBase
    {

        public bool EnableCompilationCaching { get; set; }

        public bool EnableOptimization { get; set; }

    }
}
