import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

// Angular Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatSliderModule } from '@angular/material/slider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';

// Third-party modules
import { NgxSpinnerModule } from 'ngx-spinner';
import { ToastrModule } from 'ngx-toastr';
import { NgxPaginationModule } from 'ngx-pagination';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import { NgxImageCropperModule } from 'ngx-image-cropper';
import { NgxFileDropModule } from 'ngx-file-drop';
import { NgChartsModule } from 'ng2-charts';

// App components and services
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { VideoPlayerComponent } from './components/video-player/video-player.component';
import { VideoUploadComponent } from './components/video-upload/video-upload.component';
import { VideoListComponent } from './components/video-list/video-list.component';
import { VideoCardComponent } from './components/video-card/video-card.component';
import { ChannelComponent } from './components/channel/channel.component';
import { CommentComponent } from './components/comment/comment.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { SubscriptionComponent } from './components/subscription/subscription.component';
import { LiveStreamComponent } from './components/live-stream/live-stream.component';
import { AnalyticsComponent } from './components/analytics/analytics.component';
import { SearchComponent } from './components/search/search.component';
import { NotificationsComponent } from './components/notifications/notifications.component';

// Services
import { AuthService } from './services/auth.service';
import { VideoService } from './services/video.service';
import { ChannelService } from './services/channel.service';
import { CommentService } from './services/comment.service';
import { UserService } from './services/user.service';
import { SubscriptionService } from './services/subscription.service';
import { LiveStreamService } from './services/live-stream.service';
import { AnalyticsService } from './services/analytics.service';
import { NotificationService } from './services/notification.service';
import { SignalRService } from './services/signalr.service';

// Interceptors
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';

// Guards
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    SidebarComponent,
    VideoPlayerComponent,
    VideoUploadComponent,
    VideoListComponent,
    VideoCardComponent,
    ChannelComponent,
    CommentComponent,
    UserProfileComponent,
    SubscriptionComponent,
    LiveStreamComponent,
    AnalyticsComponent,
    SearchComponent,
    NotificationsComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    
    // Angular Material
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatCardModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatMenuModule,
    MatTabsModule,
    MatChipsModule,
    MatSliderModule,
    MatProgressBarModule,
    MatBadgeModule,
    MatExpansionModule,
    MatStepperModule,
    MatCheckboxModule,
    MatRadioModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatTooltipModule,
    MatBottomSheetModule,
    
    // Third-party
    NgxSpinnerModule,
    ToastrModule.forRoot({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
    }),
    NgxPaginationModule,
    InfiniteScrollModule,
    NgxImageCropperModule,
    NgxFileDropModule,
    NgChartsModule
  ],
  providers: [
    // Services
    AuthService,
    VideoService,
    ChannelService,
    CommentService,
    UserService,
    SubscriptionService,
    LiveStreamService,
    AnalyticsService,
    NotificationService,
    SignalRService,
    
    // Guards
    AuthGuard,
    RoleGuard,
    
    // Interceptors
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
