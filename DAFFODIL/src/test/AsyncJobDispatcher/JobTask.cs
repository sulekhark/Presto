using System;

namespace AsyncJobDispatcher {
    public class JobTask {

        private static int taskIdCounter = 0;
        public JobTask() {
            this.Id = taskIdCounter++;
        }

        public string Text { get; set; }

        public int Id { get; set; }
    }
}