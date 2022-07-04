import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { subscribeOn } from 'rxjs';
import { Member } from 'src/app/_models/members';
import { MembersService } from 'src/app/_services/members.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member;

  constructor(private membersService: MembersService, private toastr: ToastrService, public presence: PresenceService) { }

  ngOnInit(): void {
  }

  addLike(member: Member){
    this.membersService.addLike(member.username).subscribe(() => {
      this.toastr.success("You Liked " + member.knownAs);
    })
  }

}
