using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.DataStructures
{
    public interface IIntervalSearchable<TKey, TEntity>
    {
         /// <summary>
        /// Intervallumos keresés
        /// </summary>
        /// <param name="from">Tól</param>
        /// <param name="to">Ig</param>
        /// <param name="fromOpen">True, akkor nagyobb vagy egyenlő mint from, ha false akkor nagyobb</param>
        /// <param name="toOpen">True, akkor kisebb vagy egyenlő mint to, ha false akkor kisebb</param>
        /// <returns>A intervallumnek megfelelő lista</returns>
        IEnumerable<TEntity> Select(TKey from, TKey to, bool fromOpen, bool toOpen);

        /// <summary>
        /// Visszadja a from értéknél kissebb elemeket
        /// </summary>
        /// <param name="from">Ennél nagyob elemeket ad vissza</param>
        /// <param name="open">True, akkor nagyobb vagy egyenlő mint from, ha false akkor nagyobb</param>
        /// <returns>From-nál nagyobb elemek listája</returns>
        IEnumerable<TEntity> SelectGreater(TKey from, bool open);


        /// <summary>
        /// Visszadja a to értéknél kisebb elemeket
        /// </summary>
        /// <param name="to">Ennél kisebb elemeket ad vissza</param>
        /// <param name="open">True, akkor kisebb vagy egyenlő mint to, ha false akkor kisebb</param>
        /// <returns>To-nál kisebb elemeket listája</returns>
        IEnumerable<TEntity> SelectLess(TKey to, bool open);


    }
}
