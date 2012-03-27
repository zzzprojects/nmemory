using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NMemory.Linq.Helpers
{
    /// <summary>
    /// 2 kifejezésfa egyezését vizsgálja
    /// Egyelőre csak a szükséges esetekre van megvalósítva.
    /// </summary>
    internal class ExpressionEqualityComparer
    {
        public static bool? Compare( Expression exp1, Expression exp2 )
        {
            if( exp1 == null && exp2 == null )
                return true;
            if( exp1 == null || exp2 == null )
                return false;
            if( exp1.NodeType != exp2.NodeType )
                return false;
            if( exp1 is BinaryExpression )
            {
                BinaryExpression b1 = exp1 as BinaryExpression;
                BinaryExpression b2 = exp2 as BinaryExpression;


                if( b1.IsLifted != b2.IsLifted || b1.IsLiftedToNull != b2.IsLiftedToNull )
                    return false;

                bool? temp = Compare( b1.Conversion, b2.Conversion );
                if( temp.HasValue == false )
                    return null;
                if( temp.Value == false )
                    return false;

                if( b1.Method != null || b2.Method != null )
                    return null;
                if( temp.HasValue == false )
                    return null;
                if( temp.Value == false )
                    return false;

                temp = Compare( b1.Left, b2.Left );
                if( temp.HasValue == false )
                    return null;
                if( temp.Value == false )
                    return false;

                temp = Compare( b1.Right, b2.Right );
                if( temp.HasValue == false )
                    return null;
                if( temp.Value == false )
                    return false;
                return true;
            }
            if( exp1.NodeType == ExpressionType.Constant )
            {
                ConstantExpression ce1 = exp1 as ConstantExpression;
                ConstantExpression ce2 = exp2 as ConstantExpression;
                return ce1.Value.Equals( ce2.Value );
            }
            if( exp1.NodeType == ExpressionType.MemberAccess )
            {
                MemberExpression me1 = exp1 as MemberExpression;
                MemberExpression me2 = exp2 as MemberExpression;
                if( me1.Member.Equals( me2.Member ) == false )
                    return false;
                bool? temp = Compare( me1.Expression, me2.Expression );
                if( temp.HasValue == false )
                    return null;
                if( temp.Value == false )
                    return false;
                return true;
            }
            if( exp1.NodeType == ExpressionType.Parameter )
            {
                ParameterExpression pe1 = exp1 as ParameterExpression;
                ParameterExpression pe2 = exp2 as ParameterExpression;
                if( pe1.Name != pe2.Name )
                    return false;
                if( pe1.Type.Equals( pe2.Type ) == false )
                    return false;
                return true;
            }
            return null;
        }
    }
}
