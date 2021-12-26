using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail
{
    class Utils
    {
        public static dynamic TryGetField(dynamic node, string fieldNode)
        {
            try
            {
                return node[fieldNode];
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
