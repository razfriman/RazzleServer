using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.CsProj;
using System.Collections.Generic;
namespace RazzleServer.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(Job.Default.With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp30));
            Add(MemoryDiagnoser.Default);
            Add(new MinimalColumnProvider());
            Add(new ConsoleLogger());
        }

        private sealed class MinimalColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary)
            {
                yield return TargetMethodColumn.Method;
                yield return StatisticColumn.Mean;
            }
        }

    }
}
