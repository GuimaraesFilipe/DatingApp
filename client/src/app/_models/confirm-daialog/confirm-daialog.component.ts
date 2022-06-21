import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-daialog',
  templateUrl: './confirm-daialog.component.html',
  styleUrls: ['./confirm-daialog.component.css']
})
export class ConfirmDaialogComponent implements OnInit {
  title: string;
  message: string;
  btnOkText: string;
  btnCancelText: string;
  result: boolean;

  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  confirm() {
    this.result = true;
    this.bsModalRef.hide();
  }
  decline() {
    this.result = false;
    this.bsModalRef.hide();
  }

}
