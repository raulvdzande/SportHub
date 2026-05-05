# Authentication Fix - Session Expires Immediately After Login

## Status: [IN PROGRESS]

### Steps:
- [ ] 1. Update SportHub.API/Application/Services/AuthService.cs: Inject IStaffTokenStore and store token after JWT generation
- [ ] 2. Restart API server (localhost:5099)
- [ ] 3. Clear browser localStorage (sporthub.token, sporthub.user)
- [ ] 4. Test login -> verify /api/workouts returns 200
- [ ] 5. Mark complete

**Root cause:** AuthService generates JWT but never stores in InMemoryStaffTokenStore. Server can't validate tokens.