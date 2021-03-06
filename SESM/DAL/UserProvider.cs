﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SESM.DTO;

namespace SESM.DAL
{
    public class UserProvider
    {
        private readonly DataContext _context;
        public UserProvider(DataContext context)
        {
            _context = context;
        }

        public EntityUser GetUser(int id)
        {
            try
            {
                return _context.Users.Find(id);
            }
            catch
            {
                return null;
            }
        }
        public List<EntityUser> GetUsers()
        {
            try
            {
                return _context.Users.ToList();
            }
            catch
            {
                return null;
            }
        }
        public EntityUser GetUser(string login)
        {
            try
            {
                EntityUser usr = _context.Users.First(u => u.Login == login);
                if (usr.Login != login)
                    return null;
                return usr;
            }
            catch
            {
                return null;
            }
        }

        public bool UserExist(int id)
        {
            try
            {
                EntityUser usr = _context.Users.First(u => u.Id == id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UserExist(string login)
        {
            try
            {
                EntityUser usr = _context.Users.First(u => u.Login == login);
                if(usr.Login != login)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateUser(EntityUser user)
        {
            try
            {
                _context.Users.Attach(user);
                _context.Entry<EntityUser>(user).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void AddUser(EntityUser user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void RemoveUser(EntityUser user)
        {
            try
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}