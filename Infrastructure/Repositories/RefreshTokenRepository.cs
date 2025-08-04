using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RefreshToken token)
        {
             _context.RefreshTokens.Remove(token);
             await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.RevokeAt == null && rt.Expire > DateTime.UtcNow && rt.Token == token);
        }

        public async Task RevokeAsync(RefreshToken token)
        {
            token.RevokeAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
