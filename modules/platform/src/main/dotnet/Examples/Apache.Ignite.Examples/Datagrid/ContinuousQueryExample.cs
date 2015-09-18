/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Event;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.ExamplesDll.Datagrid;

namespace Apache.Ignite.Examples.Datagrid
{
    /// <summary>
    /// This example demonstrates continuous query API.
    /// <para />
    /// To run the example please do the following:
    /// 1) Build the project Apache.Ignite.ExamplesDll (select it -> right-click -> Build);
    /// 2) Set this class as startup object (Apache.Ignite.Examples project -> right-click -> Properties ->
    ///     Application -> Startup object);
    /// 3) Start application (F5 or Ctrl+F5).
    /// <para />
    /// This example can be run in conjunction with standalone Apache Ignite .Net node.
    /// To start standalone node please do the following:
    /// 1) Build the project Apache.Ignite.ExamplesDll if you havent't done it yet (select it -> right-click -> Build);
    /// 2) Locate created Apache.Ignite.ExamplesDll.dll file (Apache.Ignite.ExamplesDll project -> right-click -> Properties -> Build -> Output path);
    /// 3) Locate Apache.Ignite.exe file (Apache.Ignite project -> right-click -> Properties -> Build -> Output path)
    /// Apache.Ignite.exe -IgniteHome=[path_to_IGNITE_HOME] -springConfigUrl=modules\platform\src\main\dotnet\examples\config\example-cache.xml -assembly=[path_to_Apache.Ignite.ExamplesDll.dll]
    /// </summary>
    public class ContinuousQueryExample
    {
        /// <summary>
        /// Runs the example.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            var cfg = new IgniteConfiguration
            {
                SpringConfigUrl = @"modules\platform\src\main\dotnet\examples\config\example-cache.xml",
                JvmOptions = new List<string> {"-Xms512m", "-Xmx1024m"}
            };

            using (var ignite = Ignition.Start(cfg))
            {
                Console.WriteLine();
                Console.WriteLine(">>> Cache continuous query example started.");

                var cache = ignite.GetOrCreateCache<int, string>("cache_continuous_query");

                // Clean up caches on all nodes before run.
                cache.Clear();

                const int keyCnt = 20;

                for (int i = 0; i < keyCnt; i++)
                    cache.Put(i, i.ToString());

                var qry = new ContinuousQuery<int, string>(new Listener<string>(), new ContinuousQueryFilter(15));


                // Create new continuous query.
                using (cache.QueryContinuous(qry))
                {
                    // Add a few more keys and watch more query notifications.
                    for (var i = keyCnt; i < keyCnt + 5; i++)
                        cache.Put(i, i.ToString());

                    // Wait for a while while callback is notified about remaining puts.
                    Thread.Sleep(2000);
                }
            }

            Console.WriteLine();
            Console.WriteLine(">>> Example finished, press any key to exit ...");
            Console.ReadKey();
        }

        /// <summary>
        /// Callback for continuous query example.
        /// </summary>
        private class Listener<T> : ICacheEntryEventListener<int, T>
        {
            public void OnEvent(IEnumerable<ICacheEntryEvent<int, T>> events)
            {
                foreach (var e in events)
                    Console.WriteLine("Queried entry [key=" + e.Key + ", val=" + e.Value + ']');
            }
        }
    }
}