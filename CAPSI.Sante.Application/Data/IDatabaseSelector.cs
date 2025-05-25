
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Data
{
    public interface IDatabaseSelector
    {
        IDatabaseConnection GetConnection(string dbType);
    }
}
