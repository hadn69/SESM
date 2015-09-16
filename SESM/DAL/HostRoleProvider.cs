using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SESM.DTO;

namespace SESM.DAL
{
    public class HostRoleProvider
    {
        private readonly DataContext _context;
        public HostRoleProvider(DataContext context)
        {
            _context = context;
        }

        public EntityHostRole GetHostRole(int id)
        {
            try
            {
                return _context.HostRoles.Find(id);
            }
            catch
            {
                return null;
            }
        }
        public List<EntityHostRole> GetHostRoles()
        {
            try
            {
                return _context.HostRoles.ToList();
            }
            catch
            {
                return null;
            }
        }

        public bool HostRoleExist(int id)
        {
            try
            {
                EntityHostRole hostRole = _context.HostRoles.First(item => item.Id == id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool HostRoleExist(string name)
        {
            try
            {
                EntityHostRole hostRole = _context.HostRoles.First(item => item.Name == name);
                if (hostRole.Name != name)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateHostRole(EntityHostRole hostRole)
        {
            try
            {
                _context.HostRoles.Attach(hostRole);
                _context.Entry<EntityHostRole>(hostRole).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void AddHostRole(EntityHostRole hostRole)
        {
            try
            {
                _context.HostRoles.Add(hostRole);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void RemoveHostRole(EntityHostRole hostRole)
        {
            try
            {
                _context.HostRoles.Remove(hostRole);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}