using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;

namespace CompassMobileUpdate.Services
{
    public interface IMeterService
    {
        Task<IList<Meter>> GetMetersByCustomerName(string name, string firstName, string lastName);
    }
}

