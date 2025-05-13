namespace GerGO.Utils
{
    class Validator
    {
        public static string TrimApostrpohes(string val)
        {
            if (val.StartsWith('\''))
            {
                return val.Substring(1, val.Length - 2);
            }
            return val;
        }
        public static bool IsEqual(string val1, string val2, string type)
        {
            switch (type)
            {
                case "int":
                    int i1 = int.Parse(val1);
                    int i2 = int.Parse(val2);
                    return i1 == i2;
                case "float":
                    float f1 = float.Parse(val1);
                    float f2 = float.Parse(val2);
                    return f1 == f2;
                case "string":
                    val1 = TrimApostrpohes(val1);
                    val2 = TrimApostrpohes(val2);
                    return val1.Equals(val2);
                case "date":
                    DateOnly d1 = DateOnly.Parse(val1);
                    DateOnly d2 = DateOnly.Parse(val2);
                    return d1.Equals(d2);
                case "datetime":
                    DateTime dt1 = DateTime.Parse(val1);
                    DateTime dt2 = DateTime.Parse(val2);
                    return dt1.Equals(dt2);
                case "bit":
                    return val1.Equals(val2);
                default:
                    return false;
            }
        }

        public static bool IsLess(string val1, string val2, string type)
        {
            switch (type)
            {
                case "int":
                    int i1 = int.Parse(val1);
                    int i2 = int.Parse(val2);
                    return i1 < i2;
                case "float":
                    float f1 = float.Parse(val1);
                    float f2 = float.Parse(val2);
                    return f1 < f2;
                case "string":
                    return val1.Equals(val2);
                case "date":
                    DateOnly d1 = DateOnly.Parse(val1);
                    DateOnly d2 = DateOnly.Parse(val2);
                    return d1 < d2;
                case "datetime":
                    DateTime dt1 = DateTime.Parse(val1);
                    DateTime dt2 = DateTime.Parse(val2);
                    return dt1 < dt2;
                case "bit":
                    return val1.Equals(val2);
                default:
                    return false;
            }
        }

        public static bool IsLessOrEqual(string val1, string val2, string type)
        {
            return IsLess(val1, val2, type) || IsEqual(val1, val2, type);
        }

        public static bool IsGreater(string val1, string val2, string type)
        {
            return !IsLess(val1, val2, type) && !IsEqual(val1, val2, type);
        }

        public static bool IsGreaterOrEqual(string val1, string val2, string type)
        {
            return !IsLess(val1, val2, type);
        }

    }
}
