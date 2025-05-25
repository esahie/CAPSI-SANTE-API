using System.Text;

namespace CAPSI.Sante.API.Configuration
{
    public static class SwaggerSchemaHelper
    {
        public static string GetCustomSchemaId(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            string baseName = type.Name;
            int indexOfBacktick = baseName.IndexOf('`');
            if (indexOfBacktick > 0)
            {
                baseName = baseName.Substring(0, indexOfBacktick);
            }

            StringBuilder sb = new StringBuilder(baseName);
            sb.Append("Of");

            Type[] genericArgs = type.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                Type genericArg = genericArgs[i];

                if (genericArg.IsGenericType)
                {
                    sb.Append(GetCustomSchemaId(genericArg));
                }
                else
                {
                    sb.Append(genericArg.Name);
                }

                if (i < genericArgs.Length - 1)
                {
                    sb.Append("And");
                }
            }

            return sb.ToString();
        }
    }
}
