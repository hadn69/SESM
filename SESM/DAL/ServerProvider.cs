using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceProcess;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.Helpers;

namespace SESM.DAL
{
    public class ServerProvider
    {
        private readonly DataContext _context;

        public ServerProvider(DataContext context)
        {
            _context = context;
        }


        // Port Check

        /// <summary>
        /// Check if a server port is in use by another server
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsPortAvailable(int port, EntityServer server)
        {
            try
            {
                EntityServer testServ = _context.Servers.First(serv => serv.Port == port);
                if (testServ == null)
                    return true;

                if (testServ.Id == server.Id)
                    return true;
                return false;


            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public int GetNextAvailablePort(string ip = "")
        {
            if (string.IsNullOrWhiteSpace(ip))
                ip = SEDefault.IP;

            if (!_context.Servers.Any(server => server.Ip == ip))
                return SEDefault.ServerPort;
            return _context.Servers.Where(server => server.Ip == ip).Select(server => server.Port).Max() + 1;
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

        public Dictionary<EntityServer, ServiceState> GetState(List<EntityServer> servers)
        {
            Dictionary<EntityServer, ServiceState> serversStates = new Dictionary<EntityServer, ServiceState>();

            ServiceController[] services = ServiceController.GetServices();

            foreach (EntityServer item in servers)
            {
                ServiceController service;
                try
                {
                    service = services.First(x => x.ServiceName == ServiceHelper.GetServiceName(item));
                }
                catch (Exception)
                {
                    service = null;
                }
                ServiceState state = ServiceState.Unknow;
                if (service == null)
                    state = ServiceState.Unknow;
                else if (service.Status == ServiceControllerStatus.Running)
                    state = ServiceState.Running;
                else if (service.Status == ServiceControllerStatus.StartPending)
                    state = ServiceState.Starting;
                else if (service.Status == ServiceControllerStatus.StopPending)
                    state = ServiceState.Stopping;
                else if (service.Status == ServiceControllerStatus.Stopped)
                    state = ServiceState.Stopped;

                serversStates.Add(item, state);
            }

            return serversStates;
        }

        public bool IsNameAvaialble(string name)
        {
            try
            {
                return !_context.Servers.Any(s => s.Name == name);
            }
            catch (Exception)
            {
                return false;
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
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public bool CheckAccess(int idUser, int idServer)
        {
            try
            {
                UserProvider usrPrv = new UserProvider(_context);
                EntityUser usr = usrPrv.GetUser(idUser);
                EntityServer srv = GetServer(idServer);
                if (usr.IsAdmin || srv.IsPublic)
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
                return GetAccessLevel(usr, srv);
            }
            catch (Exception)
            {
                return AccessLevel.None;
            }
        }

        public AccessLevel GetAccessLevel(EntityUser user, EntityServer server)
        {
            try
            {
                if (user == null)
                    return server.IsPublic ? AccessLevel.Guest : AccessLevel.None;

                if (user.IsAdmin)
                    return AccessLevel.SuperAdmin;
                /*
                if (server.Administrators.Contains(user))
                    return AccessLevel.Admin;
                if (server.Managers.Contains(user))
                    return AccessLevel.Manager;
                if (server.Users.Contains(user))
                    return AccessLevel.User;
                    */
                return server.IsPublic ? AccessLevel.Guest : AccessLevel.None;
            }
            catch (Exception)
            {
                return AccessLevel.None;
            }
        }

        public bool IsManagerOrAbore(AccessLevel accessLevel)
        {
            bool ret = accessLevel == AccessLevel.SuperAdmin ||
                       accessLevel == AccessLevel.Admin ||
                       accessLevel == AccessLevel.Manager;
            return ret;
        }

        public bool IsAdminOrAbore(AccessLevel accessLevel)
        {
            return accessLevel == AccessLevel.SuperAdmin ||
                   accessLevel == AccessLevel.Admin;
        }

        public List<EntityServer> GetAllServers()
        {
            return _context.Servers.ToList();
        }

        public List<EntityServer> GetAllSEServers()
        {
            return _context.Servers.Where(item => item.ServerType == EnumServerType.SpaceEngineers).ToList();
        }

        public List<EntityServer> GetAllMEServers()
        {
            return _context.Servers.Where(item => item.ServerType == EnumServerType.MedievalEngineers).ToList();
        }

        public List<EntityServer> GetAllSESEServers()
        {
            return _context.Servers.Where(item => item.UseServerExtender).ToList();
        }

        public List<EntityServer> GetAllPublicServers()
        {
            return _context.Servers.Where(item => item.IsPublic).ToList();
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
                    /*
                    listServ.AddRange(usr.AdministratorOf);
                    listServ.AddRange(usr.ManagerOf);
                    listServ.AddRange(usr.UserOf);
                    */
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
                _context.PerfEntries.RemoveRange(server.PerfEntries);
                _context.Servers.Remove(server);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public bool SecurityCheck(EntityServer server, EntityUser user)
        {
            if (user == null || server == null)
                return false;

            AccessLevel accessLevel = GetAccessLevel(user.Id, server.Id);
            if (accessLevel != AccessLevel.Guest && accessLevel != AccessLevel.User)
            {
                return true;
            }
            return false;
        }
    }

    public enum AccessLevel
    {
        SuperAdmin,
        Admin,
        Manager,
        User,
        Guest,
        None
    }
}