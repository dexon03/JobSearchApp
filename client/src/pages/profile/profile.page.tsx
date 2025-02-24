import CandidateProfileComponent from '../../components/candidate.profile.component';
import RecruiterProfileComponent from '../../components/recruiter.profile.component';
import useRole from '../../hooks/useRole';
import { Role } from '../../models/common/role.enum';

const ProfilePage = () => {

  const { role } = useRole()

  return (
    role === Role[Role.Candidate]
      ? <CandidateProfileComponent />
      : <RecruiterProfileComponent />
  );
};

export default ProfilePage;
