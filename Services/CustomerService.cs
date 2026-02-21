using PAS.Models;
using PAS.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace PAS.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CustomerService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.Include(c => c.Fields).ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers.Include(c => c.Fields)
                                           .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveQuoteAsync(Quote quote)
        {
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
        }

        public async Task<string> BuildCustomerListHtmlAsync()
        {
            var customers = await _context.Customers
                .OrderBy(c => c.CustomerBusinessName)
                .ToListAsync();

            var sb = new StringBuilder();

            sb.AppendLine("<h2>Customer List</h2>");
            sb.AppendLine($"<p>Generated: {DateTime.Now:MM/dd/yyyy}</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Business Name</th>");
            sb.AppendLine("<th>First Name</th>");
            sb.AppendLine("<th>Last Name</th>");
            sb.AppendLine("<th>City</th>");
            sb.AppendLine("<th>Phone</th>");
            sb.AppendLine("<th>Email</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var c in customers)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{c.CustomerBusinessName}</td>");
                sb.AppendLine($"<td>{c.CustomerFName}</td>");
                sb.AppendLine($"<td>{c.CustomerLName}</td>");
                sb.AppendLine($"<td>{c.CustomerCity}</td>");
                sb.AppendLine($"<td>{c.CustomerPhone}</td>");
                sb.AppendLine($"<td>{c.CustomerEmail}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        public async Task<string> BuildCustomerListPrintHtmlAsync()
        {
            var innerHtml = await BuildCustomerListHtmlAsync();

            var templatePath = Path.Combine(_env.WebRootPath, "print_customerlist.html");
            var template = await File.ReadAllTextAsync(templatePath);

            return template.Replace("{{CustomerListContent}}", innerHtml);
        }
    }
}
