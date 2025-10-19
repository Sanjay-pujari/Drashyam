import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subscription } from 'rxjs';
import { AppState } from '../../store/app.state';
import { Video } from '../../models/video.model';
import { selectVideos, selectVideoLoading, selectVideoError, selectVideoTotalCount } from '../../store/video/video.selectors';
import { loadVideos, likeVideo } from '../../store/video/video.actions';
import { VideoService } from '../../services/video.service';

@Component({
	selector: 'app-videos',
	standalone: true,
	imports: [
		CommonModule, 
		MatIconModule, 
		MatProgressSpinnerModule, 
		MatButtonModule, 
		MatChipsModule,
		MatCardModule,
		RouterModule
	],
	templateUrl: './videos.component.html',
	styleUrls: ['./videos.component.scss']
})
export class VideosComponent implements OnInit, OnDestroy {
	videos$: Observable<Video[]>;
	loading$: Observable<boolean>;
	error$: Observable<string | null>;
	totalCount$: Observable<number>;
	
	categories = [
		'All',
		'Music',
		'Gaming',
		'Education',
		'Entertainment',
		'Technology',
		'Sports',
		'News',
		'Lifestyle',
		'Travel'
	];
	
	selectedCategory = 'All';
	currentPage = 1;
	pageSize = 20;
	private subscriptions: Subscription[] = [];

	constructor(
		@Inject(Store) private store: Store<AppState>,
		private videoService: VideoService
	) {
		this.videos$ = this.store.select(selectVideos);
		this.loading$ = this.store.select(selectVideoLoading);
		this.error$ = this.store.select(selectVideoError);
		this.totalCount$ = this.store.select(selectVideoTotalCount);
	}

	ngOnInit() {
		this.loadVideos();
	}

	ngOnDestroy() {
		this.subscriptions.forEach(sub => sub.unsubscribe());
	}

	loadVideos() {
		const filter = {
			page: this.currentPage,
			pageSize: this.pageSize,
			category: this.selectedCategory === 'All' ? undefined : this.selectedCategory
		};

		this.store.dispatch(loadVideos(filter));
	}

	onCategoryChange(category: string) {
		this.selectedCategory = category;
		this.currentPage = 1;
		this.loadVideos();
	}

	loadMoreVideos() {
		this.currentPage++;
		this.loadVideos();
	}

	likeVideo(video: Video) {
		this.store.dispatch(likeVideo({ videoId: video.id, likeType: 'like' }));
	}

	formatViewCount(count: number): string {
		if (count >= 1000000) {
			return (count / 1000000).toFixed(1) + 'M';
		} else if (count >= 1000) {
			return (count / 1000).toFixed(1) + 'K';
		}
		return count.toString();
	}

	getSafeImageUrl(imageUrl: string | null | undefined): string {
		if (!imageUrl || imageUrl.includes('example.com') || imageUrl.includes('placeholder')) {
			return '/assets/default-video-thumbnail.svg';
		}
		return imageUrl;
	}

	onImageError(event: any) {
		event.target.src = '/assets/default-video-thumbnail.svg';
	}
}



