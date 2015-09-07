﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace TrueCraft.Profiling
{
    public static class Profiler
    {
        static Profiler()
        {
            Stopwatch = new Stopwatch();
            EnabledBuckets = new List<string>();
            ActiveTimers = new Stack<ActiveTimer>();
            Stopwatch.Start();
        }

        private static Stopwatch Stopwatch { get; set; }
        private static List<string> EnabledBuckets { get; set; }
        private static Stack<ActiveTimer> ActiveTimers { get; set; }

        private struct ActiveTimer
        {
            public long Started, Finished;
            public string Bucket;
        }

        [Conditional("DEBUG")]
        public static void EnableBucket(string bucket)
        {
            EnabledBuckets.Add(bucket);
        }

        [Conditional("DEBUG")]
        public static void DisableBucket(string bucket)
        {
            EnabledBuckets.Remove(bucket);
        }

        [Conditional("DEBUG")]
        public static void Start(string bucket)
        {
            ActiveTimers.Push(new ActiveTimer
            {
                Started = Stopwatch.ElapsedMilliseconds,
                Finished = -1,
                Bucket = bucket
            });
        }

        [Conditional("DEBUG")]
        public static void Done()
        {
            if (ActiveTimers.Count > 0)
            {
                var timer = ActiveTimers.Pop();
                timer.Finished = Stopwatch.ElapsedMilliseconds;
                for (int i = 0; i < EnabledBuckets.Count; i++)
                {
                    if (Match(EnabledBuckets[i], timer.Bucket))
                    {
                        Console.WriteLine("{0} took {1}ms", timer.Bucket, timer.Finished - timer.Started);
                        break;
                    }
                }
            }
        }

        private static bool Match(string mask, string value)
        {
            if (value == null)
                value = string.Empty;
            int i = 0;
            int j = 0;
            for (; j < value.Length && i < mask.Length; j++)
            {
                if (mask[i] == '?')
                    i++;
                else if (mask[i] == '*')
                {
                    i++;
                    if (i >= mask.Length)
                        return true;
                    while (++j < value.Length && value[j] != mask[i]) ;
                    if (j-- == value.Length)
                        return false;
                }
                else
                {
                    if (char.ToUpper(mask[i]) != char.ToUpper(value[j]))
                        return false;
                    i++;
                }
            }
            return i == mask.Length && j == value.Length;
        }

    }
}