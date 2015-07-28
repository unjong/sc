using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
    public static class ScExtensions
    {
        /// <summary>
        /// 에러로 부터 StackTrace 정보 추출 하여 제공
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        /// <example></example>
        public static string SxGetErrorMessage(this Exception ex)
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendLine(ex.Message);
            Exception exception = ex;
            errMsg.Append(exception.StackTrace);
            while( (exception = ex.InnerException) !=null)
            {
                errMsg.Append(exception.StackTrace);  
            }

            return errMsg.ToString();
        }

        #region GetDefaultValue

        public static T GetDefaultValue<T>()
        {
            // We want an Func<T> which returns the default.
            // Create that expression here.
            Expression<Func<T>> e = Expression.Lambda<Func<T>>(
                // The default value, always get what the *code* tells us.
                Expression.Default(typeof(T))
            );

            // Compile and return the value.
            return e.Compile()();
        }

        public static object GetDefaultValue(this Type type)
        {
            // Validate parameters.
            if (type == null) throw new ArgumentNullException("type");

            // We want an Func<object> which returns the default.
            // Create that expression here.
            Expression<Func<object>> e = Expression.Lambda<Func<object>>(
                // Have to convert to object.
                Expression.Convert(
                // The default value, always get what the *code* tells us.
                    Expression.Default(type), typeof(object)
                )
            );

            // Compile and return the value.
            return e.Compile()();
        }

        #endregion
    }   
}



/// ScmdStudentSave ( Command )
/// SpStudents => 속성 ( 프로퍼티)
/// Sf
/// Sx