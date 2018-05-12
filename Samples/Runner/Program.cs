using Kontur;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Capacity = 10;
            var sut = new Bus();

            ManualResetEvent manualReset = new ManualResetEvent(false);
            sut.Subscribe<string>(
                m =>
                {
                    Console.WriteLine(m.Payload);
                    manualReset.WaitOne();
                });

            var list = new List<bool>();

            for (var i = 0; i < Capacity * Capacity; i++)
            {

                list.Add(sut.EmitAsync("hello", new Dictionary<string, string>()).Result);
                Console.WriteLine("send");
            }

            Console.ReadLine();

            //var buffer = new BufferBlock<string>(
            //    new DataflowBlockOptions
            //    {
            //        BoundedCapacity = 10
            //    });

            //var dispatcher = new BroadcastBlock<string>(
            //    message => message,
            //    new DataflowBlockOptions
            //    {
            //        BoundedCapacity = 1
            //    });
            //BufferBlock<string> workerQueue = new BufferBlock<string>(
            //    new DataflowBlockOptions
            //    {
            //        BoundedCapacity = 10
            //    });
            //var consumerOptions = new ExecutionDataflowBlockOptions
            //{
            //    BoundedCapacity = 1
            //};
            //ManualResetEvent manual = new ManualResetEvent(false);
            //var consumer = new ActionBlock<string>(
            //    m =>
            //    {
            //        Console.WriteLine(m);
            //        manual.WaitOne();
            //    },
            //    consumerOptions);

            //var dispatcher = new ActionBlock<string>(
            //    m => 
            //    {
            //        workerQueue.SendAsync<string>(m).Wait();
            //    }, 
            //    consumerOptions);

            //buffer.LinkTo(dispatcher);
            ////dispatcher.LinkTo(consumer);
            //workerQueue.LinkTo(consumer);

            //while (true)
            //{
            //    buffer.SendAsync<string>("test").Wait();
            //    Console.WriteLine("success");
            //}


            //Console.ReadLine();
        }
    }

    internal class DoSomethingCommand
    {
    }
}
