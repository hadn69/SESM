using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceProcess;
using SESM.DTO;
using SESM.Tools;

namespace SESM.DAL
{
    public class ServerProvider
    {
        private readonly DataContext _context;

        public ServerProvider(DataContext context)
        {
            _context = context;
        }

        public bool CheckPortAvailability(int port)
        {
            try
            {
                if (_context.Servers.First(s => s.Port == port) != null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }
        public void AddAdministrator(string[] listUsers, EntityServer server)
        {
            UserProvider usrPrv = new UserProvider(_context);
            server.Administrators = null;

            foreach (string item in listUsers.Where(item => usrPrv.UserExist(item) && !server.Administrators.Contains(usrPrv.GetUser(item))))
            {
                server.Administrators.Add(usrPrv.GetUser(item));
            }
        }
        public void AddManagers(string[] listUsers, EntityServer server)
        {
            UserProvider usrPrv = new UserProvider(_context);
            server.Managers = null;

            foreach (string item in listUsers.Where(item => usrPrv.UserExist(item) && !server.Managers.Contains(usrPrv.GetUser(item))))
            {
                server.Managers.Add(usrPrv.GetUser(item));
            }
        }

        public void AddUsers(string[] listUsers, EntityServer server)
        {
            UserProvider usrPrv = new UserProvider(_context);
            server.Users = null;

            foreach (string item in listUsers.Where(item => usrPrv.UserExist(item) && !server.Users.Contains(usrPrv.GetUser(item))))
            {
                server.Users.Add(usrPrv.GetUser(item));
            }
        }
        public EntityServer GetServerByPort(int port)
        {
            try
            {
                return _context.Servers.First(s => s.Port == port);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public EntityServer GetServer(int id)
        {
            try
            {
                return _context.Servers.First(s => s.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ServiceState GetState(EntityServer server)
        {
            ServiceController[] services = ServiceController.GetServices();
            ServiceController service;
            try
            {
                service = services.First(x => x.ServiceName == ServiceHelper.GetServiceName(server));
            }
            catch (Exception)
            {
                service = null;
            }
            if (service == null)
                return ServiceState.Unknow;
            else
            {
                if (service.Status == ServiceControllerStatus.Running)
                    return ServiceState.Running;
                if (service.Status == ServiceControllerStatus.StartPending)
                    return ServiceState.Starting;
                if (service.Status == ServiceControllerStatus.StopPending)
                    return ServiceState.Stopping;
                if (service.Status == ServiceControllerStatus.Stopped)
                    return ServiceState.Stopped;
                return ServiceState.Unknow;
            }

        }

        public void UpdateServer(EntityServer server)
        {
            try
            {
                _context.Servers.Attach(server);
                _context.Entry<EntityServer>(server).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void CreateServer(EntityServer server)
        {
            try
            {
                _context.Servers.Add(server);
                //_context.Entry<EntityServer>(server).State = EntityState.Added;
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public Dictionary<int, ServiceState> GetState(List<EntityServer> serverList)
        {
            return serverList.ToDictionary(item => item.Id, GetState);
        }

        public bool CheckAccess(int idUser, int idServer)
        {
            try
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(idUser);
                EntityServer srv = GetServer(idServer);
                if (usr.IsAdmin || srv.IsPublic || srv.Administrators.Contains(usr) || srv.Managers.Contains(usr) || srv.Users.Contains(usr))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public AccessLevel GetAccessLevel(int idUser, int idServer)
        {
            try
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(idUser);
                EntityServer srv = GetServer(idServer);
                if (usr.IsAdmin)
                    return AccessLevel.SuperAdmin;

                if (srv.Administrators.Contains(usr))
                    return AccessLevel.Admin;
                if (srv.Managers.Contains(usr))
                    return AccessLevel.Manager;
                if (srv.Users.Contains(usr))
                    return AccessLevel.User;

                return AccessLevel.Guest;

            }
            catch (Exception)
            {
                return AccessLevel.Guest;
            }
        }

        public AccessLevel GetHighestAccessLevel(List<EntityServer> servers, EntityUser user)
        {
           
            if (user == null)
                return AccessLevel.Guest;
            if(user.IsAdmin)
                return AccessLevel.SuperAdmin;
            AccessLevel accessLevel = AccessLevel.Guest;
           foreach (AccessLevel itemAccessLevel in servers.Select(item => GetAccessLevel(user.Id, item.Id)))
            {
                if (accessLevel == AccessLevel.Guest 
                    && itemAccessLevel != AccessLevel.Guest)
                {
                    accessLevel = itemAccessLevel;
                }
                else if (accessLevel == AccessLevel.User
                    && itemAccessLevel != AccessLevel.Guest
                    && itemAccessLevel != AccessLevel.User)
                {
                    accessLevel = itemAccessLevel;
                }
                else if (accessLevel == AccessLevel.Manager
                    && itemAccessLevel != AccessLevel.Guest
                    && itemAccessLevel != AccessLevel.User
                    && itemAccessLevel != AccessLevel.Manager)
                {
                    accessLevel = itemAccessLevel;
                }
                else if (accessLevel == AccessLevel.Admin
                    && itemAccessLevel != AccessLevel.Guest
                    && itemAccessLevel != AccessLevel.User
                    && itemAccessLevel != AccessLevel.Manager
                    && itemAccessLevel != AccessLevel.Admin)
                {
                    accessLevel = itemAccessLevel;
                }
            }
            return accessLevel;
        }

        public List<EntityServer> GetAllServers()
        {
            return _context.Servers.ToList();
        }

        public List<EntityServer> GetServers(EntityUser user)
        {
            if (user == null)
            {
                return _context.Servers.Where(item => item.IsPublic).ToList();
            }
            try
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(user.Id);
                if (usr.IsAdmin)
                {
                    return _context.Servers.ToList();
                }
                else
                {
                    List<EntityServer> listServ = new List<EntityServer>();
                    listServ.AddRange(usr.AdministratorOf);
                    listServ.AddRange(usr.ManagerOf);
                    listServ.AddRange(usr.UserOf);
                    foreach (EntityServer item in _context.Servers.ToList().Where(item => item.IsPublic && !listServ.Contains(item)))
                    {
                        listServ.Add(item);
                    }
                    return listServ;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void RemoveServer(EntityServer server)
        {
            try
            {
                _context.Servers.Remove(server);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }

    public enum AccessLevel
    {
        SuperAdmin,
        Admin,
        Manager,
        User,
        Guest
    }
}