using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SESM.DTO;

namespace SESM.DAL
{
    public class InstanceServerRoleProvider
    {
        private readonly DataContext _context;
        public InstanceServerRoleProvider(DataContext context)
        {
            _context = context;
        }

        public EntityInstanceServerRole GetInstanceServerRole(int id)
        {
            try
            {
                return _context.InstanceServerRoles.Find(id);
            }
            catch
            {
                return null;
            }
        }
        public List<EntityInstanceServerRole> GetInstanceServerRoles()
        {
            try
            {
                return _context.InstanceServerRoles.ToList();
            }
            catch
            {
                return null;
            }
        }

        public bool InstanceServerRoleExist(int id)
        {
            try
            {
                EntityInstanceServerRole instanceServerRole = _context.InstanceServerRoles.First(item => item.Id == id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateInstanceServerRole(EntityInstanceServerRole instanceServerRole)
        {
            try
            {
                _context.InstanceServerRoles.Attach(instanceServerRole);
                _context.Entry<EntityInstanceServerRole>(instanceServerRole).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void AddInstanceServerRole(EntityInstanceServerRole instanceServerRole)
        {
            try
            {
                _context.InstanceServerRoles.Add(instanceServerRole);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void RemoveInstanceServerRole(EntityInstanceServerRole instanceServerRole)
        {
            try
            {
                _context.InstanceServerRoles.Remove(instanceServerRole);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}