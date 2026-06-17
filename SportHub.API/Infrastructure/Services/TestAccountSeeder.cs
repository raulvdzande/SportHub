using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Infrastructure.Services;

public class TestAccountSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Member> _memberHasher;
    private readonly IPasswordHasher<Instructor> _instructorHasher;

    public TestAccountSeeder(
        AppDbContext context,
        IPasswordHasher<Member> memberHasher,
        IPasswordHasher<Instructor> instructorHasher)
    {
        _context = context;
        _memberHasher = memberHasher;
        _instructorHasher = instructorHasher;
    }

    public async Task SeedAsync()
    {
        var memberEmail = "member@test.com";
        var instructorEmail = "instructor@test.com";
        var password = "Password123!";

        // Seed test member
        var existingMember = await _context.Members.FirstOrDefaultAsync(m => m.Email == memberEmail);
        if (existingMember == null)
        {
            var testMember = new Member
            {
                Id = Guid.NewGuid(),
                Email = memberEmail,
                FirstName = "Test",
                LastName = "Member",
                PhoneNumber = "0612345678"
            };

            testMember.PasswordHash = _memberHasher.HashPassword(testMember, password);

            _context.Members.Add(testMember);
            await _context.SaveChangesAsync();
        }

        // Seed test instructor
        var existingInstructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Email == instructorEmail);
        if (existingInstructor == null)
        {
            var testInstructor = new Instructor
            {
                Id = Guid.NewGuid(),
                Email = instructorEmail,
                FullName = "Test Instructor",
                PhoneNumber = "0687654321",
                CreatedAtUtc = DateTime.UtcNow
            };

            testInstructor.PasswordHash = _instructorHasher.HashPassword(testInstructor, password);

            _context.Instructors.Add(testInstructor);
            await _context.SaveChangesAsync();
        }
    }
}
