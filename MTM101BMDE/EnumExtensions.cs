using System;
using System.Collections.Generic;

namespace MTM101BaldAPI
{
	public static class EnumExtensions
	{
		private struct ExtendedEnumData
		{
			public int valueOffset;
			public List<string> Enums;

            public ExtendedEnumData(int offset)
            {
                valueOffset = offset;
                Enums = new List<string>();
            }
		}

		private static Dictionary<Type, ExtendedEnumData> ExtendedData = new Dictionary<Type, ExtendedEnumData>();

        /// <summary>
        /// ToString but for Extended enums. Only use on enums that are ints.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string ToStringExtended<T>(this T en) where T : Enum
        {
            return GetExtendedName<T>(Convert.ToInt32(en));
        }

        public static int[] GetValues<T>() where T : Enum
        {
            List<int> possibleTypes = new List<int>();
            Array values = Enum.GetValues(typeof(T));
            for (int i = 0; i < values.Length; i++)
            {
                possibleTypes.Add((int)values.GetValue(i));
            }
            if (!ExtendedData.ContainsKey(typeof(T))) return possibleTypes.ToArray();
            ExtendedEnumData data = ExtendedData[typeof(T)];
            for (int i = 0; i < data.Enums.Count; i++)
            {
                possibleTypes.Add(data.valueOffset + i);
            }
            return possibleTypes.ToArray();
        }


        /// <summary>
        /// Extends an enum, same effect could be achieved by casting an int, however this has a system to keep track of multiple enum additions from different mods to prevent conflicts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extendName"></param>
        /// <returns></returns>
		public static T ExtendEnum<T>(string extendName) where T : Enum
		{
			if (!ExtendedData.TryGetValue(typeof(T), out ExtendedEnumData dat))
			{
				dat = new ExtendedEnumData(256); //Just so nothing conflicts and mods don't break when the game updates. If this becomes problematic let me know, I'll suffer but I will try to fix it.
                ExtendedData.Add(typeof(T),dat);
            }
            if (dat.Enums.Contains(extendName))
            {
                MTM101BaldiDevAPI.Log.LogWarning("Attempted to register duplicate extended enum:" + extendName + "!");
                return GetFromExtendedName<T>(extendName);
            }
            dat.Enums.Add(extendName);
            return (T)(object)(dat.valueOffset + (dat.Enums.Count - 1));
        }

        /// <summary>
        /// Enum.GetName but with support for extended enums.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
		public static string GetExtendedName<T>(int val) where T : Enum
		{
			string theName = Enum.GetName(typeof(T), val);
            if (theName == null)
			{
                if (ExtendedData.TryGetValue(typeof(T), out ExtendedEnumData dat))
                {
                    return dat.Enums[val - dat.valueOffset];
                }
				else
				{
					return val.ToString();
				}
            }
			return theName;

        }

        /// <summary>
        /// Get an extended enum from a name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T GetFromExtendedName<T>(string name) where T : Enum
        {
            if (Enum.IsDefined(typeof(T), name))
            {
                return (T)Enum.Parse(typeof(T), name);
            }
            bool success = ExtendedData.TryGetValue(typeof(T),out ExtendedEnumData value);
            if (!success)
            {
                throw new KeyNotFoundException();
            }
            int index = value.Enums.FindIndex(x => x == name);
            if (index == -1)
            {
                throw new KeyNotFoundException();
            }
            return (T)(object)(value.valueOffset + index);
        }

        /// <summary>
        /// Returns true if the specified Enum exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool EnumWithExtendedNameExists<T>(string name) where T : Enum
        {
            if (Enum.IsDefined(typeof(T), name))
            {
                return true;
            }
            bool success = ExtendedData.TryGetValue(typeof(T), out ExtendedEnumData value);
            if (!success) return false;
            int index = value.Enums.FindIndex(x => x == name);
            if (index == -1) return false;
            return true;
        }

	}
}
