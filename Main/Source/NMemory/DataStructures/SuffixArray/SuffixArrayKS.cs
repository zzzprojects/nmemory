using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures.SuffixArray
{
    public class SuffixArrayKS<TValue> : SuffixArrayKS
    {
        public TValue Value { get; set; }
        public SuffixArrayKS(string text, TValue value)
            : base(text)
        {
            this.Value = value;
        }
    }

    public class SuffixArrayKS
    {

        public string Text { get; set; }
        public int[] SuffixArray { get; set; }
        private List<int>[] buckets = new List<int>[256];
        public EnumFullTextIndexType IndexType { get; set; }


        public SuffixArrayKS(string text)
            : this(text, EnumFullTextIndexType.None)
        {
        }

        public SuffixArrayKS(string text, EnumFullTextIndexType indexType)
        {
            if ((indexType & EnumFullTextIndexType.CaseInSensitive) == EnumFullTextIndexType.CaseInSensitive)
                this.Text = text.ToLower();
            else
                this.Text = text;

            this.IndexType = indexType;


            for (int i = 0; i < 256; i++)
                buckets[i] = new List<int>();

            CreateSuffixArray();
        }

        public bool Search(string keyword)
        {
            if ((IndexType & EnumFullTextIndexType.CaseInSensitive) == EnumFullTextIndexType.CaseInSensitive)
                keyword = keyword.ToLower();
            int min = 0;
            int max = Text.Length - 1;
            int mid = (min + max) / 2;

            int lepes = 0;
            bool success = false;
            do
            {
                mid = (min + max) / 2;

                success = true;
                for (int i = 0; i < keyword.Length; i++)
                {
                    if (Text.Length > SuffixArray[mid] + i)
                    {
                        if (keyword[i] > Text[SuffixArray[mid] + i])
                        {
                            min = mid + 1;
                            success = false;
                            break;
                        }
                        else if (keyword[i] < Text[SuffixArray[mid] + i])
                        {
                            max = mid - 1;
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        max = mid - 1;
                        success = false;
                        break;
                    }
                    lepes++;
                }

            }
            while (!success && (min <= max));

            //       Console.WriteLine(lepes);
            return success;
        }

        private void CreateSuffixArray()
        {
            string text = Text + ((char)36) + ((char)36) + ((char)36) + ((char)36);

            //int[] s = new int[text.Length];

            int n = Text.Length;

            int[] s1i = null;
            int[] s2i = null;
            int[] s0i = null;

            if ((IndexType & EnumFullTextIndexType.OnlyWords) == EnumFullTextIndexType.OnlyWords)
            {
                int words = 1;
                for (int i = 0; i < n - 1; i++)
                    if (text[i] == ' ')
                        words++;
                int[] wordIndex = new int[words];

                int k = 1;
                wordIndex[0] = 0;
                for (int i = 0; i < n-1; i++)
                {
                    if (text[i] == ' ')
                    {
                        wordIndex[k] = i+1;
                        k++;
                    }
                }

                int lengthS1 = (int)Math.Ceiling((words - 1) / 3.0);
                int lengthS2 = (int)Math.Ceiling((words - 2) / 3.0);
                int lengthS0 = (int)Math.Ceiling((words) / 3.0);

                s1i = new int[lengthS1];
                s2i = new int[lengthS2];
                s0i = new int[lengthS0];

                for (int i = 0; i < words; i += 3)
                    s0i[i / 3] = wordIndex[i];
                for (int i = 1; i < words; i += 3)
                    s1i[(i - 1) / 3] = wordIndex[i];
                for (int i = 2; i < words; i += 3)
                    s2i[(i - 2) / 3] = wordIndex[i];


            }
            else
            {

                int lengthS1 = (int)Math.Ceiling((n - 1) / 3.0);
                int lengthS2 = (int)Math.Ceiling((n - 2) / 3.0);
                int lengthS0 = (int)Math.Ceiling((n) / 3.0);

                s1i = new int[lengthS1];
                s2i = new int[lengthS2];
                s0i = new int[lengthS0];

                for (int i = 0; i < Text.Length; i += 3)
                    s0i[i / 3] = i;
                for (int i = 1; i < Text.Length - 2; i += 3)
                    s1i[(i - 1) / 3] = i;
                for (int i = 2; i < Text.Length - 2; i += 3)
                    s2i[(i - 2) / 3] = i;
            }

            int[] s12 = new int[s1i.Length + s2i.Length];
            Array.Copy(s1i, 0, s12, 0, s1i.Length);
            Array.Copy(s2i, 0, s12, s1i.Length, s2i.Length);

            RadixSort(text, s12);
            RadixSort(text, s0i);

            SuffixArray = Merge(text, s12, s0i);

        }

        public string[] GetSuffix()
        {
            string[] s = new string[SuffixArray.Length];
            for (int i = 0; i < SuffixArray.Length; i++)
            {
                s[i] = Text.Substring(SuffixArray[i], Text.Length - SuffixArray[i]);
            }
            return s;
        }


        public int[] Merge(string text, int[] s1, int[] s2)
        {
            StringComparer comparer = new StringComparer();
            int[] r = new int[s1.Length + s2.Length];

            int p1 = 0; int p2 = 0;
            while (p1 < s1.Length && p2 < s2.Length)
            {
                if (comparer.Compare(text.Substring(s1[p1], 3), text.Substring(s2[p2], 3)) > 0)
                {
                    r[p1 + p2] = s2[p2]; p2++;
                }
                else
                {
                    r[p1 + p2] = s1[p1];
                    p1++;
                }
            }

            while (p1 < s1.Length)
            {
                r[p1 + p2] = s1[p1];
                p1++;
            }

            while (p2 < s2.Length)
            {
                r[p1 + p2] = s2[p2];
                p2++;
            }

            return r;
        }


        public void RadixSort(string text, int[] a)
        {

            int k = 2;
            int n = a.Length;

            while (k >= 0)
            {
                for (int i = 0; i < 256; i++)
                    buckets[i].Clear();

                foreach (int i in a)
                {
                    char ch = text[i + k];
                    buckets[(byte)ch].Add(i);
                }

                int f = 0;
                foreach (var l in buckets)
                    foreach (var p in l)
                    {
                        a[f] = p;
                        f++;
                    }
                k--;
            }


        }
    }

    public class StringComparer : Comparer<string>
    {
        public override int Compare(string x, string y)
        {
            for (int i = 0; i < Math.Min(x.Length, x.Length); i++)
            {
                if (x[i] < y[i])
                    return -1;
                else if (x[i] > y[i])
                    return 1;
            }
            return 0;
        }
    }

    public enum EnumFullTextIndexType
    {
        None = 0,
        CaseInSensitive = 1,
        OnlyWords = 2,
    }

}
