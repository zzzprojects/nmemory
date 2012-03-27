using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Execution.Optimization.Modifiers;
using NMemory.Tables;
using System.Diagnostics;
using NMemory.Diagnostics;

namespace NMemory.StoredProcedures
{
    /// <summary>
    /// A tárolt eljárást magában foglaló helytartó oszály, kívülről nem látszik
    /// A felhasználó csak egy Func objektumot kap vissza
    /// A public láthatóság más szerelvényekben elhelyezkedő tesztek miatt szükséges
    /// </summary>
    public class StoredProcedure<TEntity>
    {
        #region Adattagok
        //private Func<Dictionary<string,object>, IEnumerable<TEntity>> privateProcedure;
        private Expression Expression { get; set; }
        public ILoggingPort LoggingPort { get; set; }
        private Database source;
        #endregion

        #region Tulajdonságok
        //public IEnumerable<TEntity> Procedure( Dictionary<string, object> param )
        //{
        //    //return this.privateProcedure( param );
        //}
        #endregion

        #region Constructor
        public StoredProcedure( IQueryable<TEntity> query, Database source )
        {
            this.Expression = query.Expression;
            this.source = source;
            CreateExecutionPlan();
        }
        public StoredProcedure( Expression expression, Database source )
        {
            this.Expression = expression;
            this.source = source;
            CreateExecutionPlan();
        }
        #endregion

        #region Segédmetódusok
        private void CreateExecutionPlan()
        {
            //Stopwatch stopper = new Stopwatch();
            //stopper.Start();

            //Expression modified = ExpressionTreeHelper.CallModifiers( this.Expression,
            //       ExpressionTreeHelper.QueryPlanModifiers(this.LoggingPort, this.source).Concat(
            //       ExpressionTreeHelper.StoredProcedureModifiers( typeof( IEnumerable<TEntity> ) ) ) );

            //this.privateProcedure = ( modified as LambdaExpression ).Compile() as Func<Dictionary<string, object>, IEnumerable<TEntity>>;

            //stopper.Stop();
            //string s = string.Format( "Expression compiled in {0} ms.", stopper.ElapsedMilliseconds );
            //lock( Console.Out )
            //{
            //    Console.ForegroundColor = ConsoleColor.DarkYellow;
            //    Console.WriteLine( s );
            //    Console.ResetColor();
            //}
        }
        #endregion
    }
}
