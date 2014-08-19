
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

        public void RemoveEntry(EntityPerfEntry perfEntry)
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

        public void RemoveEntries(IEnumerable<EntityPerfEntry> perfEntries)
        {
            try
            {
                _context.PerfEntries.RemoveRange(perfEntries);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

    }
}