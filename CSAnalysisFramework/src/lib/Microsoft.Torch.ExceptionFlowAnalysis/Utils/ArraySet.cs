using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Microsoft.Torch.ExceptionFlowAnalysis.Utils
{
    public class ArraySet : IEnumerable<int[]>
    {
        protected HashSet<int[]> arrSet;
        protected readonly int arrSize;
        

        public ArraySet(int sz)
        {
            arrSet = new HashSet<int[]>();
            arrSize = sz;
        }

        public int GetArrSize()
        {
            return arrSize;
        }

        public int Size()
        {
            return arrSet.Count;
        }

        public bool Add(int[] arr)
        {
            if (arr.Length == arrSize)
            {
                return arrSet.Add(arr);
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            arrSet.Clear();
        }

        public bool Contains(int posn, int valIdx)
        {
            IEnumerable<int[]> selQuery = from arr in arrSet
                                          where arr[posn] == valIdx
                                          select arr;
            int count = 0;
            foreach (int[] arr in selQuery)
            {
                count++;
            }
            return (count != 0);
        }

        public bool Contains(int[] posns, int[] valIdxs)
        {
            throw new NotImplementedException();
        }

        public bool Contains(int[] arr)
        {
            throw new NotImplementedException();
        }
       
      
        public object Project(int posn)
        {
            throw new NotImplementedException();
        }

        public object Project(int[] posns)
        {
            throw new NotImplementedException();
        }
      
        public HashSet<int[]> Select(int posn, int valIdx)
        {
            IEnumerable<int[]> selQuery = from arr in arrSet
                                          where arr[posn] == valIdx
                                          select arr;
            HashSet<int[]> selection = new HashSet<int[]>();
            foreach (int[] arr in selQuery)
            {
                selection.Add(arr);
            }
            return selection;
        }

        public object Select(int[] posns, int[] valIdxs)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<int[]> GetEnumerator()
        {
            foreach (int[] arr in arrSet)
            {
                yield return arr;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
