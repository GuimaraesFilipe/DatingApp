import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photo: Photo[];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotoForApproval();
  }

  getPhotoForApproval() {
    this.adminService.getPhotoForApproval().subscribe(photo => {
      this.photo = photo;
    })
  }

  approvePhoto(photoId) {
    this.adminService.ApprovePhoto(photoId).subscribe(() => {
    this.photo.splice(this.photo.findIndex(p => p.id === photoId), 1);
    })
  }
  
  rejectPhoto(photoId) {
    this.adminService.rejectPhoto(photoId).subscribe(() => {
      this.photo.splice(this.photo.findIndex(p => p.id === photoId), 1);
    })
  }
      
}
