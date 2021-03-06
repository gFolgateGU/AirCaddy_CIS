﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirCaddy.Data.CustomDataModels;

namespace AirCaddy.Data.Repositories
{
    public interface IPrivilegeRepository
    {
        Task<bool> ExistsCourseRequestAsync(string courseName, string courseAddress);

        Task AddCourseRequestAsync(PrivilegeRequest privilegeRequest);

        Task<IEnumerable<UserPrivilegeRequest>> GetAllPendingRequestsAsync();

        Task<IEnumerable<PrivilegeRequest>> GetAllPendingRequestsForUserAsync(string userId);
    }

    public class PrivilegeRepository : BaseRepository, IPrivilegeRepository
    {
        public PrivilegeRepository () : base() { }

        public async Task<bool> ExistsCourseRequestAsync(string courseName, string courseAddress)
        {
            var courseRequest = await _dataEntities.PrivilegeRequests
                                                .FirstOrDefaultAsync
                                                (cr => cr.GolfCourseName.Contains(courseName) && cr.GolfCourseAddress.Contains(courseAddress));
            return courseRequest != null;
        }

        public async Task AddCourseRequestAsync(PrivilegeRequest privilegeRequest)
        {
            _dataEntities.PrivilegeRequests.Add(privilegeRequest);
            await _dataEntities.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserPrivilegeRequest>> GetAllPendingRequestsAsync()
        {
            const string query =
                @"SELECT AspNetUsers.UserName, PrivilegeRequests.Id, PrivilegeRequests.GolfCourseName, PrivilegeRequests.GolfCourseAddress, PrivilegeRequests.CoursePhoneNumber,
	                     PrivilegeRequests.GolfCourseType, PrivilegeRequests.Reason, PrivilegeRequests.Verified
	                     FROM AspNetUsers
	                     INNER JOIN PrivilegeRequests on AspNetUsers.Id = PrivilegeRequests.UserId
	                     WHERE PrivilegeRequests.Verified = 0
	                     GROUP BY AspNetUsers.Id, AspNetUsers.UserName, PrivilegeRequests.Id, PrivilegeRequests.GolfCourseName, PrivilegeRequests.GolfCourseAddress, PrivilegeRequests.CoursePhoneNumber,
	                     PrivilegeRequests.GolfCourseType, PrivilegeRequests.Reason, PrivilegeRequests.Verified";

            var pendingRequests = await _dataEntities.Database.SqlQuery<UserPrivilegeRequest>(query).ToListAsync();
            return pendingRequests;
        }

        public async Task<IEnumerable<PrivilegeRequest>> GetAllPendingRequestsForUserAsync(string userId)
        {
            var userPendingRequests =
                await _dataEntities.PrivilegeRequests.Where(pr => pr.UserId.Contains(userId) && pr.Verified == false).ToListAsync();
            return userPendingRequests;
        }
    }
}
