using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SESM.DTO;

namespace SESM.DAL
{
    public class ServerRoleProvider
    {
        private readonly DataContext _context;
        public ServerRoleProvider(DataContext context)
        {
            _context = context;
        }

        public EntityServerRole GetServerRole(int id)
        {
            try
            {
                return _context.ServerRoles.Find(id);
            }
            catch
            {
                return null;
            }
        }
        public List<EntityServerRole> GetServerRoles()
        {
            try
            {
                return _context.ServerRoles.ToList();
            }
            catch
            {
                return null;
            }
        }

        public bool ServerRoleExist(int id)
        {
            try
            {
                EntityServerRole serverRole = _context.ServerRoles.First(item => item.Id == id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ServerRoleExist(string name)
        {
            try
            {
                EntityServerRole serverRole = _context.ServerRoles.First(item => item.Name == name);
                if (serverRole.Name != name)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateServerRole(EntityServerRole serverRole)
        {
            try
            {
                _context.ServerRoles.Attach(serverRole);
                _context.Entry<EntityServerRole>(serverRole).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void AddServerRole(EntityServerRole serverRole)
        {
            try
            {
                _context.ServerRoles.Add(serverRole);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void RemoveServerRole(EntityServerRole serverRole)
        {
            try
            {
                _context.ServerRoles.Remove(serverRole);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}