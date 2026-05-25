using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Models;

namespace QuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantsController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public ParticipantsController(QuizDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/participants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Participant>>> GetParticipants()
        {
            return await _context.Participants
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.TimeTaken)
                .ToListAsync();
        }

        // ✅ GET: api/participants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Participant>> GetParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);

            if (participant == null)
                return NotFound();

            return participant;
        }

        // ✅ POST: api/participants (Register participant)
        [HttpPost]
        public async Task<ActionResult<Participant>> PostParticipant(Participant participant)
        {
            var temp = _context.Participants
                .Where(x => x.Name == participant.Name
                && x.Email == participant.Email)
                .FirstOrDefault();

            if (temp == null)
            {
                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();
            }
            else
                participant = temp;

            return Ok(participant);
        }

            // ✅ PUT: api/participants/5 (Update full details)
            [HttpPut("{id}")]
        public async Task<IActionResult> PutParticipant(int id, ParticipantResult _participantResult)
        {
            if (id != _participantResult.ParticipantId)
            {
                return BadRequest();
            }

            // get all current details of the record, then update with quiz results
            Participant participant = _context.Participants.Find(id);
            participant.Score = _participantResult.Score;
            participant.TimeTaken = _participantResult.TimeTaken;

            _context.Entry(participant).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE: api/participants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null)
                return NotFound();

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ 🎯 IMPORTANT: Submit Quiz Result
        [HttpPost("result")]
        public async Task<IActionResult> SubmitResult(ParticipantResult result)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantId == result.ParticipantId);

            if (participant == null)
                return NotFound("Participant not found");

            participant.Score = result.Score;
            participant.TimeTaken = result.TimeTaken;

            await _context.SaveChangesAsync();

            return Ok(participant);
        }
    }
}