export interface VideoProcessingProgress {
  id: number;
  videoId: number;
  status: string;
  progressPercentage: number;
  currentStep?: string;
  errorMessage?: string;
  createdAt: string;
  updatedAt: string;
}
