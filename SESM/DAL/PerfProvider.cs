
using System.Collections.Generic;
using SESM.DTO;

namespace SESM.DAL
{
    public class PerfProvider
    {
        private readonly DataContext _context;

        public PerfProvider(DataContext context)
        {
            _context = context;
        }

        public void RemoveServer(EntityPerfEntry perfEntry)
        {
            try
            {
                _context.PerfEntries.Remove(perfEntry);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void RemoveServer(IEnumerable<EntityPerfEntry> perfEntries)
        {
            try
            {
                foreach (EntityPerfEntry item in perfEntries)
                {
                    _context.PerfEntries.Remove(item);
                }
                
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

    }
}