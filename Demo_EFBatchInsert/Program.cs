using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_EFBatchInsert
{
    class Program
    {

        static void Main(string[] args)
        {
            var testData = GetSample(3000);
            Demo("Share Context，逐筆 SaveChange", TestRun01, testData);
            Demo("Not Share Context，逐筆 SaveChange", TestRun02, testData);
            Demo("Share Context，一次 SaveChange", TestRun03, testData);
            Demo("Share Context， Batch SaveChange", TestRun04, testData);
            Demo("Not Share Context， Batch SaveChange", TestRun05, testData);
            Console.ReadLine();
        }

        private static IEnumerable<Users> GetSample(int ItemCount)
        {
            var randomchar = "0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ";
            var rand = new Random();

            for (int i = 1; i <= ItemCount; i++)
            {
                yield return new Users
                {
                    UID = i,
                    Id = i,
                    Name = new string(Enumerable.Repeat(randomchar, 10).Select(x => x[rand.Next(x.Length)]).ToArray())
                };
            }
        }

        private static void Demo(string title, Action<IEnumerable<Users>> fun, IEnumerable<Users> userData)
        {
            var sw = new Stopwatch();
            sw.Start();
            fun(userData);
            sw.Stop();
            Console.WriteLine($"資料筆數：{userData.Count()}\t花費時間：{sw.ElapsedMilliseconds}\t測試說明：{title}");
        }

        /// <summary>
        /// 逐筆Add，Share Context，逐筆SaveChange
        /// </summary>
        /// <param name="ItemData"></param>
        private static void TestRun01(IEnumerable<Users> ItemData)
        {
            TSQL2019Entities dbContext = new TSQL2019Entities();

            foreach (var item in ItemData)
            {
                dbContext.Users.Add(item);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 逐筆Add，在 Foreach 裡逐筆SaveChange
        /// </summary>
        /// <param name="ItemData"></param>
        private static void TestRun02(IEnumerable<Users> ItemData)
        {
            foreach (var item in ItemData)
            {
                TSQL2019Entities dbContext = new TSQL2019Entities();
                dbContext.Users.Add(item);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 逐筆Add，一次SaveChange
        /// </summary>
        /// <param name="ItemData"></param>
        private static void TestRun03(IEnumerable<Users> ItemData)
        {
            TSQL2019Entities dbContext = new TSQL2019Entities();

            foreach (var item in ItemData)
            {
                dbContext.Users.Add(item);
            }
            dbContext.SaveChanges();
        }

        /// <summary>
        /// 批次Add，批次SaveChange
        /// </summary>
        /// <param name="ItemData"></param>
        private static void TestRun04(IEnumerable<Users> ItemData)
        {
            TSQL2019Entities dbContext = new TSQL2019Entities();
            var batchCount = 100;
            var i = 0;

            foreach (var item in ItemData)
            {
                dbContext.Users.Add(item);

                if (i % batchCount == 0)
                    dbContext.SaveChanges();
            }
            dbContext.SaveChanges();
        }

        /// <summary>
        /// 批次Add，批次SaveChange，最後關閉DBContext
        /// </summary>
        /// <param name="ItemData"></param>
        private static void TestRun05(IEnumerable<Users> ItemData)
        {
            var batchCount = 100;
            var skipCount = 0;

            var _ItemData = ItemData.ToArray();
            for (int i = 1; i <= (ItemData.Count() / batchCount); i++)
            {
                using (TSQL2019Entities dbContext = new TSQL2019Entities())
                {
                    for (int j = skipCount; j < batchCount * i; j++)
                    {
                        dbContext.Users.Add(_ItemData[j]);
                        skipCount = j+1;
                    }
                    dbContext.SaveChanges();
                }
            }
        }


    }
}
