using System;

namespace AssignmentS2P2
{
    sealed class UserTransaction
    {
        // This class saves user's transaction details such as date, time started, time ended, time spent.
        // Single object reference to a transaction
        internal static UserTransaction transactionSession;

        // Readonly prevents changes to date and start time during a transaction
        internal readonly DateTime TransactionDate;
        internal readonly DateTime SessionStart;
        internal DateTime SessionEnd;
        internal TimeSpan SessionDuration;

        internal UserTransaction()
        {
            this.TransactionDate = DateTime.Today;
            this.SessionStart = DateTime.Now;
        }

        internal void TransactionEndEvent()
        {
            this.SessionEnd = DateTime.Now;
            this.SessionDuration = SessionEnd.Subtract(SessionStart);
        }
    }
}
