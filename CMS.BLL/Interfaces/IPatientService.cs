/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.BLL.Interfaces
{
    interface IPatientService
    {
    }
}
*/
using CMS.Models.Entities;

namespace CMS.BLL.Interfaces;

public interface IPatientService
{
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(int id, Patient updatedPatient); // جديد
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm); // جديد
    Task<Patient> GetHistoryAsync(int id);                    // جديد

    Task<IEnumerable<Patient>> GetAllAsync();
}