import {Component, Input, OnInit, ViewChild} from '@angular/core';
import {ModalDirective} from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-yes-no-modal',
  templateUrl: './yes-no-modal.component.html',
  styleUrls: ['./yes-no-modal.component.css']
})
export class YesNoModalComponent implements OnInit {

  @ViewChild('yesNoModal') public yesNoModal!: ModalDirective;

  @Input() type = 'info';
  @Input() title = '';
  @Input() body = '';
  @Input() yesBtnText = 'Да';
  @Input() noBtnText = 'Нет';

  constructor() { }

  ngOnInit(): void {
  }

  public showAsync(data = null): Promise<any> {
    return new Promise<any>((resolve, reject) => {
      this.yesNoModal.show();
      this.onYesClick = () => {
        this.yesNoModal.hide();
        resolve(data);
      };
      this.onNoClick = () => {
        this.yesNoModal.hide();
        reject(data);
      };
    });
  }

  onYesClick(): any {}
  onNoClick(): any {}

}