using System;
using System.Collections.Generic;
using System.Linq;
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
    }   
}



/// ScmdStudentSave ( Command )
/// SpStudents => 속성 ( 프로퍼티)
/// Sf
/// Sx